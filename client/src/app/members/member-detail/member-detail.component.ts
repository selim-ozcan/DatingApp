import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  NgxGalleryAnimation,
  NgxGalleryImage,
  NgxGalleryOptions,
} from '@kolkov/ngx-gallery';
import { setTime } from 'ngx-bootstrap/chronos/utils/date-setters';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs') memberTabs: TabsetComponent | undefined;
  member: Member | undefined;
  activeTab: TabDirective | undefined;

  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];

  user: User | undefined;

  constructor(
    public membersService: MembersService,
    private messageService: MessageService,
    private route: ActivatedRoute,
    private cd: ChangeDetectorRef,
    private toast: ToastrService,
    public presenceService: PresenceService,
    private accountService: AccountService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: (user) => {
        if (user) this.user = user;
      },
    });

    this.route.data.subscribe({
      next: (data) => {
        this.member = data['member'];
        this.galleryImages = this.getImages();
      },
    });
  }
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngAfterViewInit() {
    this.route.queryParams.subscribe({
      next: (params) => {
        if (params['tab']) {
          this.selectTab(params['tab']);
          this.cd.detectChanges();
        }
      },
    });
  }

  ngOnInit(): void {
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];
  }

  getImages() {
    if (!this.member) return [];
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url,
      });
    }
    return imageUrls;
  }

  loadMember() {
    const username = this.route.snapshot.paramMap.get('username');
    if (!username) return;
    this.membersService.getMember(username).subscribe({
      next: (member: Member) => {
        if (!member) return;
        this.member = member;
        this.galleryImages = this.getImages();
      },
    });
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const tab = this.memberTabs.tabs.find((x) => x.heading === heading);
      if (tab) tab.active = true;
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.user && this.member) {
      this.messageService.createHubConnection(this.user, this.member?.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  likeUser() {
    if (!this.member?.userName) return;
    this.membersService.addLike(this.member?.userName).subscribe({
      next: (response) => {
        this.toast.success('You liked ' + this.member?.userName);
      },
    });
  }
}
