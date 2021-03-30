import socket
import time

HOST = "localhost"
PORT = 8080


def sendcommand():
    global HOST, PORT, command
    final_command = "exit"
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))
    while True:
        print("1 - pitch,\n2 - roll,\n3 - yaw,\n4 - throttle,\n5 - takeoff,\n6 - land,\n7 - joystick")
        val = input("enter command:\n")
        if val == "1":
            command = "direct_vehicle_control:pitch,0.1"
        elif val == "2":
            command = "direct_vehicle_control:roll,0.2"
        elif val == "3":
            command = "direct_vehicle_control:yaw,0.5"
        elif val == "4":
            command = "direct_vehicle_control:throttle,0.1"
        elif val == "-1":
            command = "direct_vehicle_control:pitch,-0.1"
        elif val == "-2":
            command = "direct_vehicle_control:roll,-0.2"
        elif val == "-3":
            command = "direct_vehicle_control:yaw,-0.5"
        elif val == "-4":
            command = "direct_vehicle_control:throttle,-0.1"

        elif val == "5":
            command = "takeoff_command"
        elif val == "6":
            command = "land_command"
        elif val == "7":
            command = "joystick"
        elif val == "0":
            command = "exit"

        sock.sendall(bytes(command + "\n", "utf-8"))
        data = sock.recv(1024)
        print(data)
        time.sleep(1)
        if data == b'ok':
            print('command was ')
        elif data == b'exiting\r\n':
            print('exited')
        else:
            print('lol')
        # sock.close()


commands = ["yaw,-0.2", "yaw,0.2", "yaw,-0.2"]
commands2 = ["pitch,0.5", "throttle,0.02", "pitch,-0.5"]

if __name__ == "__main__":
    sendcommand()
