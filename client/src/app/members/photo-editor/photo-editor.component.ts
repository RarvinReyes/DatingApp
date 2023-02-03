import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { ToastrService } from 'ngx-toastr';
import { map, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member | undefined;
  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  user: User | undefined;

  constructor(private accountService: AccountService, private membersService: MembersService,
    private toastr: ToastrService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => { if (user) this.user = user; }
    });
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  setMainPhoto(photo: Photo) {
    this.membersService.setMainPhoto(photo.id)
      .subscribe({
        next: () => {
          if (this.user && this.member) {
            this.user.photoUrl = photo.url;
            this.accountService.setCurrentUser(this.user);
            this.member.photoUrl = photo.url;
            this.member.photos.forEach(f => {
              if (f.isMain) { f.isMain = false; }
              if (f.id == photo.id) { f.isMain = true; }
              this.toastr.success("Photo set to main");
            })
          }
        },
      });
  }

  deletePhoto(photo: Photo) {
    this.membersService.deletePhoto(photo.id)
      .subscribe({
        next: () => {
          if (this.member) {
            this.member.photos = this.member.photos.filter(x => x.id != photo.id);
            this.toastr.success("Photo deleted");
          }
        }
      })
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + "users/add-photo",
      authToken: 'Bearer ' + this.user?.token,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, res, status, headers) => {
      if (res) {
        const photo = JSON.parse(res);
        this.member?.photos.push(photo);
      }
    }
  }

}
