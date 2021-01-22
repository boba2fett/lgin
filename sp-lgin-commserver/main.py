#!/usr/bin/env python3
from api import LginApi
import time
from http.server import HTTPServer
from server import Server
import ssl
import json
import socket
import sys
import os

if len(sys.argv)>1 and sys.argv[1]=="dev":
    with open('settings.dev.json', 'r') as json_file:
        config = json.load(json_file)
    os.environ['sp-commserver-env'] = "dev"
else:
    with open('settings.json', 'r') as json_file:
        config = json.load(json_file)
    os.environ['sp-commserver-env'] = "prod"

class HTTPServerV6(HTTPServer):
    address_family = socket.AF_INET6

if __name__ == '__main__':
    api = LginApi()
    httpd = HTTPServer((config["HOST_NAME"], config["PORT_NUMBER"]), Server)
    httpd.api = api
    #httpd.socket = ssl.wrap_socket (httpd.socket, 
    #    keyfile=f"/etc/letsencrypt/live/{config['domain']}/privkey.pem", 
    #    certfile=f"/etc/letsencrypt/live/{config['domain']}/fullchain.pem", server_side=True)
    print('Server Starts - %s:%s' % (config["HOST_NAME"], config["PORT_NUMBER"]))
    httpd.serve_forever()
