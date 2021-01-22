import { AccountService, AlertService } from '@app/_services';
import { Component, OnInit , VERSION, ViewChild, ElementRef, AfterViewChecked } from "@angular/core";
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-startpage',
  templateUrl: './startpage.component.html',
  styleUrls: ['./startpage.component.less']
})



export class StartpageComponent implements OnInit, AfterViewChecked {

  commandHistory = [];
  historyPosition = 0;

  shellContent: Cmd[];
  shellPrompt = "startpage@shell#";
  shellCmd = "";
  categories;

  @ViewChild('target') private myScrollContainer: ElementRef;
  @ViewChild('inpute') private myInput: ElementRef;

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
    this.loadCategories();
    this.updatePrompt();
  }

  loadCategories(){
    this.accountService.getCmd("ls","").pipe().subscribe({next: (response: any) => {
      console.log(response.stdout);
      console.log(JSON.parse(response.stdout));
      this.categories = JSON.parse(response.stdout);
    },
    error: error => {}
  });
  }

  ngAfterViewChecked() {        
    this.scrollToBottom();
    this.myInput.nativeElement.focus();      
  }

  scrollToBottom(): void {
    try {
      this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { } 
  }

  insertStdout(command: string, stdout:string) {
    //let content = this.escapeHtml(stdout);
    if (this.shellContent){
      //this.shellContent.reverse();
      this.shellContent.push({
        stdout: stdout,
        cmd: command
      });
      //this.shellContent.reverse();
    }
    else {
      this.shellContent = [{
        stdout: stdout.trim(),
        cmd: command.trim()
      }];
    }
  }

  featureShell(command) {
    if (command === "") {
        return;
    }
    if (/^\s*\?\s*[^\s]+.*$/.test(command)) {
        let cmd=command.match(/^\s*\?\s*([^\s]+).*$/)[1];
        let args="";
        if (/^\s*\?\s*[^\s]+\s+.*$/.test(command)) {
            args=command.match(/^\s*\?\s*[^\s]+\s+(.*)$/)[1];
        }

        this.accountService.getCmd(cmd,args).pipe().subscribe({next: (response: any) => {
            if (response.href) {
                window.location.href = response.href
            }
            this.insertStdout(command,response.stdout);
            if (response.stderr) {
              this.insertStdout(command,response.stderr);
            }
            if (cmd=="add"){
              this.loadCategories();
            }
        },
        error: error => {
          this.insertStdout(command,error);
        }
      });
    } else {
        window.location.href = "https://duckduckgo.com/?q=" + command;
    }
  }

  featureHint() {
    if (this.shellCmd.trim().length === 0) return;  // field is empty -> nothing to complete
    return;
  }

  genPrompt() {
    return "startpage@shell#";
  }

  escapeHtml(string) {
    return string
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
  }

  updatePrompt() {
    this.shellPrompt = this.genPrompt();
  }

  onShellCmdKeyDown(event) {
    switch (event.key) {
        case "Enter":
          this.featureShell(this.shellCmd);
          this.insertToHistory(this.shellCmd);
            this.shellCmd = "";
            break;
        case "ArrowUp":
            if (this.historyPosition > 0) {
              this.historyPosition--;
              this.shellCmd = this.commandHistory[this.historyPosition];
            }
            break;
        case "ArrowDown":
            if (this.historyPosition >= this.commandHistory.length) {
                break;
            }
            this.historyPosition++;
            if (this.historyPosition === this.commandHistory.length) {
              this.shellCmd = "";
            } else {
              this.shellCmd = this.commandHistory[this.historyPosition];
            }
            break;
        case 'Tab':
            event.preventDefault();
            this.featureHint();
            break;
    }
  }

  insertToHistory(cmd) {
    this.commandHistory.push(cmd);
    this.historyPosition = this.commandHistory.length;
  }


}

class Cmd {
  stdout: string;
  cmd: string;
}