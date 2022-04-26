using System;

namespace FanControl.CommanderPro
{
    internal class CommanderProProtocolConstants
    {
        public const Int32 COMMAND_SIZE = 64;
        public const Int32 RESPONSE_SIZE = 16;

        public const Int32 READ_STATUS = 0x01;
        public const Int32 READ_FIRMWARE_VERSION = 0x02;
        public const Int32 READ_DEVICE_ID = 0x03;
        public const Int32 WRITE_DEVICE_ID = 0x04;
        public const Int32 START_FIRMWARE_UPDATE = 0x05;
        public const Int32 READ_BOOTLOADER_VERSION = 0x06;
        public const Int32 WRITE_TEST_FLAG = 0x07;
        public const Int32 READ_TEMPERATURE_MASK = 0x10;
        public const Int32 READ_TEMPERATURE_VALUE = 0x11;
        public const Int32 READ_VOLTAGE_VALUE = 0x12;
        public const Int32 READ_FAN_MASK = 0x20;
        public const Int32 READ_FAN_SPEED = 0x21;
        public const Int32 READ_FAN_POWER = 0x22;
        public const Int32 WRITE_FAN_POWER = 0x23;
        public const Int32 WRITE_FAN_SPEED = 0x24;
        public const Int32 WRITE_FAN_CURVE = 0x25;
        public const Int32 WRITE_FAN_EXTERNAL_TEMP = 0x26;
        public const Int32 WRITE_FAN_FORCE_THREE_PIN_MODE = 0x27;
        public const Int32 WRITE_FAN_DETECTION_TYPE = 0x28;
        public const Int32 READ_FAN_DETECTION_TYPE = 0x29;
        public const Int32 READ_LED_STRIP_MASK = 0x30;
        public const Int32 WRITE_LED_RGB_VALUE = 0x31;
        public const Int32 WRITE_LED_COLOR_VALUES = 0x32;
        public const Int32 WRITE_LED_TRIGGER = 0x33;
        public const Int32 WRITE_LED_CLEAR = 0x34;
        public const Int32 WRITE_LED_GROUP_SET = 0x35;
        public const Int32 WRITE_LED_EXTERNAL_TEMP = 0x36;
        public const Int32 WRITE_LED_GROUPS_CLEAR = 0x37;
        public const Int32 WRITE_LED_MODE = 0x38;
        public const Int32 WRITE_LED_BRIGHTNESS = 0x39;
        public const Int32 WRITE_LED_COUNT = 0x3A;
        public const Int32 WRITE_LED_PORT_TYPE = 0x3B;
        public const Int32 PROTOCOL_RESPONSE_OK = 0x00;
        public const Int32 PROTOCOL_RESPONSE_ERROR = 0x01;

        public const Int32 Fan_Auto_Mode = 0;
        public const Int32 Fan_3pin_Mode = 1;
        public const Int32 Fan_4pin_Mode = 2;
    }

    internal class CommanderCoreProtocolConstants
    {
        public const Int32 COMMAND_SIZE = 97;
        public const Int32 RESPONSE_SIZE = 97;

        public static Byte[] COMMAND_WAKE = { 0x01, 0x03, 0x00, 0x02 };
        public static Byte[] COMMAND_SLEEP = { 0x01, 0x03, 0x00, 0x01 };
        public static Byte[] COMMAND_RESET = { 0x05, 0x01, 0x00 };
        public static Byte[] COMMAND_SET_MODE = { 0x0d, 0x00 };
        public static Byte[] COMMAND_READ = { 0x08, 0x00 };

        public static Byte[] READ_FIRMWARE_VERSION = { 0x02, 0x13 };

        public static Byte[] MODE_CONNECTED = { 0x1a };
        public static Byte[] MODE_GET_SPEEDS = { 0x17 };

        public static Byte[] DATA_TYPE_CONNECTED = { 0x09, 0x00 };
        public static Byte[] DATA_TYPE_SPEEDS = { 0x06, 0x00 };

        //public static Object _CMD_WRITE = (0x06, 0x00);

        //public static Object _MODE_LED_COUNT = ValueTuple.Create(0x20);

        //public static Object _MODE_GET_TEMPS = ValueTuple.Create(0x21);

        //public static Object _MODE_HW_SPEED_MODE = (0x60, 0x6d);

        //public static Object _MODE_HW_FIXED_PERCENT = (0x61, 0x6d);

        //public static Object _DATA_TYPE_LED_COUNT = (0x0f, 0x00);

        //public static Object _DATA_TYPE_TEMPS = (0x10, 0x00);

        //public static Object _DATA_TYPE_HW_SPEED_MODE = (0x03, 0x00);

        //public static Object _DATA_TYPE_HW_FIXED_PERCENT = (0x04, 0x00);
    }
}