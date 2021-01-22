from api import LginApi
import os

from http.server import BaseHTTPRequestHandler

from urllib.parse import urlparse, parse_qs
from response.commHandler import CommHandler
from response.badRequestHandler import BadRequestHandler
from response.forbiddenRequestHandler import ForbiddenRequestHandler
import json
import jwt


class Server(BaseHTTPRequestHandler):


    def __init__(self,*args):
        if os.getenv('sp-commserver-env')=="dev":
            with open('settings.dev.json', 'r') as json_file:
                self.config = json.load(json_file)
        else:
            with open('settings.json', 'r') as json_file:
                self.config = json.load(json_file)
        BaseHTTPRequestHandler.__init__(self,*args)

    def do_OPTIONS(self):
        self.send_response(200, "ok")
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, OPTIONS')
        self.send_header("Access-Control-Allow-Headers", "X-Requested-With")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        self.send_header("Access-Control-Allow-Headers", "Authorization")
        self.end_headers()

    def do_HEAD(self):
        return

    def do_GET(self):
        parsed_path = urlparse(self.path)
        real_path = parsed_path.path
        params = parse_qs(self.path.replace(real_path+"?", ""))
        #print(params)
        args=""
        try:
            args=params['args'][0]
        except:
            pass
        
        real_path = real_path.replace("/commserver", "")
        
        #print(self.headers.get('Authorization'))

        jwtoken = self.headers.get('Authorization').replace("Bearer ","")
        try:
            jwtoken = jwt.decode(jwtoken,self.config["jwtkey"],algorithms=['HS256'])
            #print(jwtoken)
            if self.server.api.hasGroup(jwtoken["unique_name"]):
                #print(f"{self.client_address}: {real_path} {args}")
                #print(os.listdir("routes"))
                
                if real_path[1::] in os.listdir("routes"): # prefix is still /
                    
                    handler = CommHandler()
                    handler.find(real_path, jwtoken["unique_name"], args)
                else: 
                    handler = BadRequestHandler()
            else: 
                handler = BadRequestHandler()
        except Exception as e:
            print(e)
            handler = BadRequestHandler()
 
        self.respond({
            'handler': handler
        })

    def do_POST(self):
        parsed_path = urlparse(self.path)
        real_path = parsed_path.path

        content_len = int(self.headers.get('Content-Length'))
        post_body = self.rfile.read(content_len)
        
        args=""
        try:
            args=json.loads(post_body)['args']
        except:
            pass
        
        real_path = real_path.replace("/commserver", "")
        
        #print(self.headers.get('Authorization'))
        try:
            jwtoken = self.headers.get('Authorization').replace("Bearer ","")
            jwtoken = jwt.decode(jwtoken,self.config["jwtkey"],algorithms=['HS256'])
            #print(jwtoken)
            if self.server.api.hasGroup(jwtoken["unique_name"]):
                #print(f"{self.client_address}: {real_path} {args}")
                #print(os.listdir("routes"))
                
                if real_path[1::] in os.listdir("routes"): # prefix is still /
                    
                    handler = CommHandler()
                    handler.find(real_path, jwtoken["unique_name"], args)
                else: 
                    handler = BadRequestHandler()
            else: 
                handler = BadRequestHandler()
        except Exception as e:
            print(e)
            handler = ForbiddenRequestHandler()
 
        self.respond({
            'handler': handler
        })

    def handle_http(self, handler):
        status_code = handler.getStatus()

        self.send_response(status_code)

        content = handler.getContents()
        self.send_header('Content-type', handler.getContentType())
        self.send_header('Access-Control-Allow-Origin', '*') # proxy support

        self.end_headers()

        return bytes(content, 'UTF-8')

    def respond(self, opts):
        response = self.handle_http(opts['handler'])
        self.wfile.write(response)
