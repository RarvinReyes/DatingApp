import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  model: any = {};

  constructor(private accountService: AccountService, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  register() {
    this.accountService.register(this.model).subscribe({
      next: () => {
        this.cancel();
        this.toastr.success('user registered');
      },
      error: err => {
        if (err.error.errors.Username)
          this.toastr.error(err.error.errors.Username[0]);
        else if (err.error.errors.Password)
          this.toastr.error(err.error.errors.Password[0]);
      }
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
