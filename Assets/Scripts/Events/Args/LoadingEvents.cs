using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ShowLoadingScreenEvent
{
    public bool start;

    public ShowLoadingScreenEvent(bool _start)
    {
        start = _start;
    }
}

public class WorldCreatedEvent
{
    public World world;

    public WorldCreatedEvent(World _world)
    {
        world = _world;
    }
}

public class GameLoaderStateEvent
{
    public GameLoadingState oldState;
    public GameLoadingState newState;

    public GameLoaderStateEvent(GameLoadingState _oldState, GameLoadingState _newState)
    {
        oldState = _oldState;
        newState = _newState;
    }
}

public class GetLoadingState
{
    public GameLoadingState state;
    public string stateText;

    public GetLoadingState()
    {
        state = GameLoadingState.error;
        stateText = "No response";
    }
}