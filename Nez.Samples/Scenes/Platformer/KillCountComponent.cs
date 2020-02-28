namespace Nez.Samples
{
    public class KillCountComponent: Component
    {
        public int kills;
        public string playerName;
        
        public KillCountComponent(string name)
        {
            this.playerName = name;
            this.kills = 0;
        }
    }
}