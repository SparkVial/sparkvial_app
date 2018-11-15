import serial
import time
import struct

SCAN = 1 << 5
READ = 1 << 6
SET_INTERVAL = 1 << 7

print("0\t0")

ser = serial.Serial('/dev/ttyS3', 115200)  # 115200 = bits/second
time.sleep(3)  # Give arduino time to boot up

ser.write([SCAN | 2, 1])  # 2 = 32 bit timestamps
                          # 1 = 1ms resolution
time.sleep(0.5)  # Give arduino time to respond to the scan

# print("Scan results:", )
ser.read(ser.in_waiting)

# print("Adjusting interval to 25ms")
ser.write([SET_INTERVAL, 25, 0, 0, 0])

# print("Listening...")
while True:
	ser.write([READ | 31])  # 31 = up to 31 samples per response
	count = ser.read(1)[0]

	for i in range(count):
		idx = ser.read(1)[0]
		ts = ser.read(4)
		v1 = ser.read(4)
		v2 = ser.read(4)
		# print(f"#{idx}: t={int.from_bytes(ts, 'little'):5}, a={struct.unpack('<f', v1)[0]:0.04f}, b={struct.unpack('<f', v2)[0]:0.04f}")
		print(f"{int.from_bytes(ts, 'little')}\t{struct.unpack('<f', v1)[0]:0.04f}")
