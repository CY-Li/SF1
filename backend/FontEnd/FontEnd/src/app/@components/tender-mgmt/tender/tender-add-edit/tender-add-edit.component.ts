import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TenderService } from '../../../../@services/tender-mgmt/tender.service';
import { IPostTender } from '../../../../@entities/tender-mgmt/tender.model';
import { MemberAddEditComponent } from '../../../member-mgmt/member/member-add-edit/member-add-edit.component';

@Component({
  selector: 'app-tender-add-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatButtonModule
  ],
  templateUrl: './tender-add-edit.component.html',
  styleUrl: './tender-add-edit.component.scss'
})
export class TenderAddEditComponent {
  // 判斷是新增還是修改，true:修改、false:新增
  // isData: boolean = false;

  // 定義FormGroup
  addeditformGroup: FormGroup;

  constructor(
    private _tenderService: TenderService,
    private _formBuilder: FormBuilder,
    private _dialogRef: MatDialogRef<MemberAddEditComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
  ) {
    // 定義頁面查詢條件
    this.addeditformGroup = this._formBuilder.group({
      mm_id: '',
      roscam_name: this._formBuilder.control('', [Validators.maxLength(25)]),//標會名稱
      roscam_type: ''//標會種類
    })
  }

  onFormSubmit() {
    if (this.addeditformGroup.valid) {
      const formData: IPostTender = this.addeditformGroup.getRawValue();
      formData.mm_id = 1;
      this._tenderService
        .postTender(formData).subscribe({
          next: (data) => {
            if (data && data.result && data.returnStatus == 1) {
              alert("新增成功");
              this._dialogRef.close();
              // this.myDataSource = new MatTableDataSource(data.result?.gridRESPResult);
            } else {
              console.log(data);
              alert("新增失敗");
            }
          }, error: (error) => console.log(error)
        })
    }
  }

  ngOnInit(): void {
    this.addeditformGroup.patchValue(this.data);
  }
}
