import { Component, OnInit } from '@angular/core';
import { BlobOptions } from 'buffer';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { PaginatedResult, Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-members-list',
  templateUrl: './members-list.component.html',
  styleUrls: ['./members-list.component.css'],
})
export class MembersListComponent implements OnInit {
  members: Member[] = [];
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' },
  ];

  constructor(public membersService: MembersService) {}

  ngOnInit(): void {
    this.userParams = this.membersService.getUserParams();
    this.loadMembers();
  }

  loadMembers() {
    if (!this.userParams) return;
    this.membersService.setUserParams(this.userParams);
    this.membersService.getMembers(this.userParams).subscribe({
      next: (response: PaginatedResult<Member[]>) => {
        if (response.result && response.pagination) {
          this.members = response.result;
          this.pagination = response.pagination;
        }
      },
    });
  }

  pageChanged(event: PageChangedEvent) {
    if (!this.userParams) return;
    this.userParams.pageNumber = event.page;
    this.userParams.pageSize = event.itemsPerPage;
    this.loadMembers();
  }

  resetFilters() {
    this.membersService.resetUserParams();
    this.userParams = this.membersService.getUserParams();
    this.loadMembers();
  }
}
