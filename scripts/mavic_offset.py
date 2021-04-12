import socket
import time
import os
import subprocess


HOST = "localhost"
PORT = 8080


def sendcommand():
    global HOST, PORT
    command = 0
    final_command = "exit"
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect((HOST, PORT))
    while True:
        print("1 - x,\n2 - y,\n3 - z,\n4 - rotate,\n5 - takeoff,\n6 - land,\n7 - joystick")
        val = input("enter command:\n")
        if val == "1":
            command = "set_position_offset:x,0.5"
        elif val == "2":
            command = "set_position_offset:y,0.5"
        elif val == "3":
            command = "set_position_offset:z,0.5"
        elif val == "4":
            command = "set_relative_heading:relative_heading,3.14"
        elif val == "-1":
            command = "set_position_offset:x,-0.5"
        elif val == "-2":
            command = "set_position_offset:y,-0.5"
        elif val == "-3":
            command = "set_position_offset:z,-0.5"
        elif val == "-4":
            command = "set_relative_heading:relative_heading,-3.14"
        elif val == "5":
            command = "takeoff_command"
        elif val == "6":
            command = "land_command"
        elif val == "7":
            command = "joystick"
        elif val == "0":
            command = "exit"
        # sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # sock.connect((HOST, PORT))

        sock.sendall(("{}\n".format(command)).encode('utf-8'))
        # else:
        #     sock.sendall(("{}\n".format(final_command)).encode('utf-8'))
        data = sock.recv(1024)
        print(data)
        time.sleep(1)
        if data == b'profit\r\n':
            print('ok')
        elif data == b'exiting\r\n':
            print('exited')
        else:
            print('lol')
        # sock.close()


sendcommand()

