using UnityEngine;
using System.Collections;
using System;
using NRand;

public enum GameLoadingState
{
    starting,
    generateWorld, //start without save
    loadWorld, //start with save
    instanciatePlayer,
    prerender, //render region around the player
    loaded,
    error,
}

public class GameLoader : MonoBehaviour
{
    [SerializeField] WorldGeneratorSettings m_settings = new WorldGeneratorSettings();
    [SerializeField] GameObject m_playerPrefab = null;

    WorldGenerator m_generator = null;

    GameLoadingState m_state = GameLoadingState.starting;

    private void Update()
    {
        switch(m_state)
        {
            case GameLoadingState.starting:
                StartWorldCreation();
                break;
            case GameLoadingState.loadWorld:
                ProcessLoadWorld();
                break;
            case GameLoadingState.generateWorld:
                ProcessGenerateWorld();
                break;
            case GameLoadingState.instanciatePlayer:
                SpawnPlayer();
                break;
            case GameLoadingState.prerender:
                Prerender();
                break;
            case GameLoadingState.loaded:
                Loaded();
                break;
            case GameLoadingState.error:

                break;
            default:
                Debug.LogError("Unknow loading state");
                break;
        }
    }

    void ChangeState(GameLoadingState newState)
    {
        GameLoadingState oldState = m_state;
        m_state = newState;

        Event<GameLoaderStateEvent>.Broadcast(new GameLoaderStateEvent(oldState, newState));
    }

    void StartWorldCreation()
    {
        //for now we only have a generator, no save to load

        ChangeState(GameLoadingState.generateWorld);
        InitGenerateWorld();
    }

    void InitGenerateWorld()
    {
        m_generator = new WorldGenerator();
        m_generator.Generate(m_settings);
    }

    void ProcessGenerateWorld()
    {
        if(m_generator.state == WorldGenerator.State.finished)
        {
            Event<WorldCreatedEvent>.Broadcast(new WorldCreatedEvent(m_generator.world));
            ChangeState(GameLoadingState.instanciatePlayer);
        }
        else if(m_generator.state == WorldGenerator.State.error)
        {
            ChangeState(GameLoadingState.error);
        }
    }

    void InitLoadWorld()
    {
        throw new NotImplementedException();
    }

    void ProcessLoadWorld()
    {
        throw new NotImplementedException();
    }

    void SpawnPlayer()
    {
        if(m_playerPrefab == null)
        {
            ChangeState(GameLoadingState.error);
            return;
        }

        var pos = SearchSpawnLocation();

        var player = Instantiate(m_playerPrefab);

        player.transform.position = pos;

        ChangeState(GameLoadingState.prerender);
    }

    Vector3Int SearchSpawnLocation()
    {
        var rand = new MT19937((uint)m_settings.seed);

        var world = PlaceholderWorld.instance.world;

        var genSize = new UniformIntDistribution(0, world.size);

        var mat = new Matrix<BlockData>(3, 3, 3);

        for (int i = 0; i < 50; i++)
        {
            int x = genSize.Next(rand);
            int z = genSize.Next(rand);
            int height = world.GetTopBlockHeight(x, z) + 1;
            int botomHeight = world.GetBottomBlockHeight(x, z);

            bool centerBlockOk = false;
            while (height > botomHeight)
            {
                world.GetLocalMatrix(x - 1, height - 1, z - 1, mat);

                var centerBlock = mat.Get(1, 0, 1);
                var centerType = BlockTypeList.instance.Get(centerBlock.id);
                if (centerType.IsEmpty() || !centerType.IsFaceFull(BlockFace.Up, centerBlock.data))
                {
                    height--;
                    continue;
                }

                centerBlockOk = true;
                break;
            }

            if (!centerBlockOk)
                continue;

            int nbHole = 0;
            int nbWall = 0;

            for (int a = 0; a < 3; a++) //x
            {
                for(int b = 0; b < 3; b++) //z
                {
                    if (a == 1 && b == 1)
                        continue;

                    var groundBlock = mat.Get(a, 0, b);
                    var groundType = BlockTypeList.instance.Get(groundBlock.id);
                    var wallType = BlockTypeList.instance.Get(mat.Get(a, 1, b).id);
                    var wall2Type = BlockTypeList.instance.Get(mat.Get(a, 2, b).id);

                    if(!wallType.IsEmpty() || !wall2Type.IsEmpty())
                    {
                        nbWall++;
                        continue;
                    }

                    if (!groundType.IsFaceFull(BlockFace.Up, groundBlock.data))
                        nbHole++;
                }
            }

            if (nbHole + nbWall > 4)
                continue; //too close from stuff

            return new Vector3Int(x, height, z);
        }

        //if we came here, no valid position found
        int h = world.GetTopBlockHeight(0, 0) + 1;

        return new Vector3Int(0, h, 0);
    }

    void Prerender()
    {
        var renderState = new GetChunkRenderedCountEvent();
        Event<GetChunkRenderedCountEvent>.Broadcast(renderState);

        //at least 9 chunks and half of the draw chunks

        if (renderState.rederedChunkNb < 9)
            return;
        if (renderState.rederedChunkNb * 2 < renderState.totalChunkNb)
            return;

        ChangeState(GameLoadingState.loaded);
    }

    void Loaded()
    {
        gameObject.SetActive(false);
    }
}
