namespace libsv {
    public enum Command {
        Heartbeat = 0 << 5,
        Scan = 1 << 5,

        ReadStream = 2 << 5,
        GetBacklog = 2 << 5,

        AdjustInterval = 4 << 5,

        Write = 6 << 5
    }

    public enum DeviceClass {
        Buffer = 0,
        Peripheral = 1
    }

    public enum FieldType {
        ByteArray = 0,
        Int = 1,
        Float = 2,
        Double = 3
    }
}
