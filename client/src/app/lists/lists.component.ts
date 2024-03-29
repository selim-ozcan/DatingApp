import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { Member } from '../_models/member';
import { Pagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css'],
})
export class ListsComponent implements OnInit {
  members: Member[] | undefined;
  pagination: Pagination | undefined;
  predicate = 'liked';
  pageNumber = 1;
  pageSize = 5;

  constructor(private memberService: MembersService) {}

  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes() {
    this.memberService
      .getLikes(this.predicate, this.pageNumber, this.pageSize)
      .subscribe({
        next: (response) => {
          this.members = response.result;
          this.pagination = response.pagination;
        },
      });
  }

  pageChanged(event: PageChangedEvent) {
    this.pageNumber = event.page;
    this.pageSize = event.itemsPerPage;
    this.loadLikes();
  }
}
