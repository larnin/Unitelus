using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Gamestate
{
    static Gamestate m_instance = null;
    public static Gamestate instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new Gamestate();
            return m_instance;
        }
    }

    public bool paused { get; set; }
}
