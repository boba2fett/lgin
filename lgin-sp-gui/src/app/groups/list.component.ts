import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';

import { AccountService } from '@app/_services';

@Component({ templateUrl: 'list.component.html' ,
    styleUrls: ['list.component.less'] })
export class ListComponent implements OnInit {
    groups = null;

    constructor(private accountService: AccountService) {}

    ngOnInit() {
        this.accountService.getAllGroups()
            .pipe(first())
            .subscribe(groups => this.groups = groups);
    }

    deleteGroup(id: string) {
        // const group = this.groups.find(x => x.id === id);
        // group.isDeleting = true;
        // this.accountService.deleteGroup(id)
        //     .pipe(first())
        //     .subscribe(() => this.groups = this.groups.filter(x => x.id !== id));
    }
}