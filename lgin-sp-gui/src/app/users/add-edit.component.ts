import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, NgControl } from '@angular/forms';
import { first } from 'rxjs/operators';

import { AccountService, AlertService } from '@app/_services';
import { Observable } from 'rxjs';
import { User, Group, GroupCheck } from '@app/_models';
import { group } from '@angular/animations';

@Component({ templateUrl: 'add-edit.component.html' })
export class AddEditComponent implements OnInit {
    form: FormGroup;
    id: string;
    isAddMode: boolean;
    loading = false;
    submitted = false;
    groups: GroupCheck[];
    allGroups: Group[];
    userGroups: Group[];

    constructor(
        private formBuilder: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private accountService: AccountService,
        private alertService: AlertService
    ) {}

    ngOnInit() {
        this.id = this.route.snapshot.params['id'];
        this.isAddMode = !this.id;
        
        // password not required in edit mode
        const passwordValidators = [Validators.minLength(10)];
        if (this.isAddMode) {
            passwordValidators.push(Validators.required);
        }

        this.form = this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', passwordValidators]
        });

        if (!this.isAddMode) {
            this.accountService.getById(this.id)
                .pipe(first())
                .subscribe(x => this.form.patchValue(x));
        }
        if (!this.isAddMode){
            this.accountService.getGroups(this.id)
                .pipe().subscribe((groups) => {
                    this.userGroups=groups;
                    this.calcEnabled();
                });
            
            this.accountService.getAllGroups()
                .pipe().subscribe((groups) => {
                    this.allGroups=groups;
                    this.calcEnabled();
                });
        }
    }

    private calcEnabled()
    {
        if( this.allGroups && this.userGroups)
        {
            this.groups=[];
            this.allGroups.forEach(group => {
                if (this.userGroups.find(g => g.id === group.id))
                {
                    this.groups.push({
                        id: group.id,
                        name: group.name,
                        enabled: true
                    });
                }
                else{
                    this.groups.push({
                        id: group.id,
                        name: group.name,
                        enabled: false
                    });
                }
            });
        }
    }

    changeStatus(id:string, event)
    {
        let group = this.groups.find(g=>g.id === id);
        group.enabled = !group.enabled
    }

    // convenience getter for easy access to form fields
    get f() { return this.form.controls; }

    onSubmit() {
        this.submitted = true;

        // reset alerts on submit
        this.alertService.clear();

        // stop here if form is invalid
        if (this.form.invalid) {
            return;
        }

        this.loading = true;
        if (this.isAddMode) {
            this.createUser();
        } else {
            this.calcDiff();
            this.updateUser();
        }
    }

    private createUser() {
        this.accountService.register(this.form.value)
            .pipe(first())
            .subscribe({
                next: () => {
                    this.alertService.success('User added successfully', { keepAfterRouteChange: true });
                    this.router.navigate(['../'], { relativeTo: this.route });
                },
                error: error => {
                    this.alertService.error(error);
                    this.loading = false;
                }
            });
    }

    private updateUser() {
        this.accountService.update(this.id, this.form.value)
            .pipe(first())
            .subscribe({
                next: () => {
                    this.alertService.success('Update successful', { keepAfterRouteChange: true });
                    this.router.navigate(['../../'], { relativeTo: this.route });
                },
                error: error => {
                    this.alertService.error(error);
                    this.loading = false;
                }
            });
    }

    private calcDiff()
    {
        let add = [];
        let rm = [];
        this.groups.forEach(group => {
            if (!this.userGroups.find(g => g.id===group.id) && group.enabled) {
                add.push(group.id);
            }
            if (this.userGroups.find(g => g.id===group.id) && !group.enabled) {
                rm.push(group.id);
            }
        });
        this.accountService.addGroups(this.id, add)
        .pipe(first())
            .subscribe({
                next: () => {
                    this.alertService.success('Group Update successful', { keepAfterRouteChange: true });
                },
                error: error => {
                    this.alertService.error(error);
                    this.loading = false;
                }
            });
        this.accountService.rmGroups(this.id, rm)
        .pipe(first())
            .subscribe({
                next: () => {
                    this.alertService.success('Group Update successful', { keepAfterRouteChange: true });
                },
                error: error => {
                    this.alertService.error(error);
                    this.loading = false;
                }
            });
    }
}