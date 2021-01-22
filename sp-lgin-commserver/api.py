import os
import json
import requests
from requests.sessions import default_headers



class LginApi():
    def __init__(self):
        if os.getenv('sp-commserver-env')=="dev":
            with open('settings.dev.json', 'r') as json_file:
                self.config = json.load(json_file)
        else:
            with open('settings.json', 'r') as json_file:
                self.config = json.load(json_file)
        self.token = "asdf"
        self.auth()
        

        
# POST http://localhost:4000/api/users/authenticate HTTP/1.1
# content-type: application/json

# {
#     "username": "Imperator",
#     "password": "ThisIsMadnessNoThisIsSparta!"
# }

# GET http://localhost:4000/api/users/1/groups HTTP/1.1
# content-type: application/json
# Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEiLCJuYmYiOjE2MTAxMzgwOTIsImV4cCI6MTYxMDc0Mjg5MiwiaWF0IjoxNjEwMTM4MDkyfQ.GH4oPTzUP9K5muUKY-A3vGItcBplezDeXCV6THvg-Uo

    def ensureCreated(self):
        try:
            r=requests.get(f"{self.config['api_server']}/groups", headers={"Authorization":f"Bearer {self.token}"})
            commGroupExists = False
            if r.status_code == 200:
                try:
                    groups = json.loads(r.text)
                    for group in groups:
                        if group["name"] == "commServer":
                            commGroupExists = True
                except:
                    pass
            if not commGroupExists:
                r=requests.post(f"{self.config['api_server']}/groups/add", headers={"Authorization":f"Bearer {self.token}"},json=
                {
                    "name": "commServer"
                }
                )
                print("created commServer Group")
        except Exception as ex:
            print("Error while checking for commGroup")
            print(ex)

    def auth(self):
        try:
            r=requests.post(f"{self.config['api_server']}/users/authenticate", json=
            {
                "username": "Internal",
                "password": self.config["api_secret"]
            }
            )
            if r.status_code == 200:
                try:
                    self.token = json.loads(r.text)["token"]
                    self.ensureCreated()
                except:
                    pass
            return
        except Exception as ex:
            print("Error while auth")
            print(ex)

    def hasGroup(self, id, secondTry=False):
        try:
            r=requests.get(f"{self.config['api_server']}/users/{id}/groups", headers={"Authorization":f"Bearer {self.token}"})
            if r.status_code == 200:
                try:
                    groups = json.loads(r.text)
                    for group in groups:
                        if group["name"] == "Admin": #Admin
                            return True
                        if group["name"] == "commServer": #internal
                            return True
                except:
                    pass
            if r.status_code != 200 and not secondTry:
                self.auth()
                return self.hasGroup(id, secondTry=True)
            return False
        except Exception as ex:
            print("error retrieving hasGroup")
            print(ex)
            print("Trying Auth")
            self.auth()

        return False