from response.requestHandler import RequestHandler
import subprocess
import os
import json

class CommHandler(RequestHandler):
    def __init__(self):
        super().__init__()
        self.contentType = 'application/json'
        self.contents = 'lol'
        if os.getenv('sp-commserver-env')=="dev":
            with open('settings.dev.json', 'r') as json_file:
                config = json.load(json_file)
                self.token = config["href_token"]
        else:
            with open('settings.json', 'r') as json_file:
                config = json.load(json_file)
                self.token = config["href_token"]

    def find(self, routeData, id, args):
        try:
            content=dict()
            routeData = f"routes{routeData}"
            self.setStatus(200)
            cmdline=[routeData]+[id]+args.split(" ")
            process = subprocess.Popen(cmdline, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            output, perror = process.communicate()
            content["stdout"] = output.decode("utf-8")
            if self.token in content["stdout"]:
                content["stdout"] = content["stdout"].replace(self.token,"")
                content["href"] = content["stdout"]
            if perror:
                content["stderr"] = perror.decode('utf-8')
            self.contents=json.dumps(content)
            return True
        except Exception as e:
            self.setStatus(500)
            self.contents=str(e)
            return False
