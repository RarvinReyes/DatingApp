import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { Photo } from 'src/app/_models/photo';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  member: Member = {} as Member;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];

  constructor(private memberService: MembersService, private route: ActivatedRoute, private messageService: MessageService) { }

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        if (this.member) {
          this.member.photos.forEach((photo: Photo) => {
            this.galleryImages.push({
              small: photo.url,
              medium: photo.url,
              big: photo.url,
            })
          });
        }
      }
    })

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab']);
        console.log(this.memberTabs);
      }
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];
  }

  loadMessage() {
    if (this.member) {
      this.messageService.GetMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      });
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading == 'Messages') {
      this.loadMessage();
    }
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
  }
}
