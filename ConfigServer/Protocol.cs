namespace ConfigServer
{
    struct Header
    {
        int lenght;
        int srcType;    //0 아무값....
        int srcCode;    //0 똥값....
        int dstType;    //0 쓰레기값....
        int dstCode;    //0 가비지값....
    }
    struct Body
    {
        Command comm;
        byte[] data;
    }

    enum Command
    {
        MATCH_REQUEST,
        MATCH_COMPLET,
        LATENCY,
        HEALTH_CHECK,           //Config Server가 사용할 수 있는 것
        MSLIST_REQUEST          //Config Server가 사용할 수 있는 것
    }

    enum DestinationType:int    //필요없는 거..................
    {
        MATCHING_SERVER = 0,
        MATCHING_CLIENT,
        ROOM_MANAGER,
        PACKET_GENERATOR,
        MONITORING_SERVER
    };


}
