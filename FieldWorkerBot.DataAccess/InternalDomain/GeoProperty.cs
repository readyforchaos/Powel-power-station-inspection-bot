namespace FieldWorkerBot.DataAccess.InternalDomain
{
    public class GeoProperty
    {
        public string State;
        public bool Selected;

        public GeoProperty()
        {
            State = "ERROR";
            Selected = false;
        }
    }
}