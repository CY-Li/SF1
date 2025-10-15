import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'app-confirm-dialog-common',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatDialogModule,
    MatButtonModule
  ],
  templateUrl: './confirm-dialog-common.component.html',
  styleUrl: './confirm-dialog-common.component.scss'
})
export class ConfirmDialogCommonComponent {
  confirmDialogResult: boolean = false;

  constructor(
    private _dialogRef: MatDialogRef<ConfirmDialogCommonComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
  ) { 
    console.log(data);
  }

  onOkClick(): void {
    this.confirmDialogResult = true;
    this._dialogRef.close(this.confirmDialogResult);
  }

  onNoClick(): void {
    this.confirmDialogResult = false;
    this._dialogRef.close(this.confirmDialogResult);
  }
}
