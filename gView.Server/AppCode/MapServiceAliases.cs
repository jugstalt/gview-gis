namespace gView.Server.AppCode
{
    class MapServiceAlias : MapService
    {
        string _serviceName;

        //public MapServiceAlias(string alias, MapServiceType type, string serviceName)
        //    : base(alias, type)
        //{
        //    _serviceName = serviceName;
        //}

        public string ServiceName
        {
            get { return _serviceName; }
        }
    }
}
