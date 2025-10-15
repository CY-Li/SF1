import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { BlooperImagesService } from './blooper-images.service';

interface ImageData {
  url: string;
  file: File | null; // 假設 file 可以是 File 物件或 null
}

@Component({
  selector: 'app-blooper-images',
  standalone: true,
  imports: [
        CommonModule,
        MatTableModule,
        MatPaginatorModule,
        MatIconModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
  ],
  templateUrl: './blooper-images.component.html',
  styleUrl: './blooper-images.component.scss'
})
export class BlooperImagesComponent {

  imageForm: FormGroup;
  images: ImageData[] = [];

  constructor(
    private _service: BlooperImagesService,//後端API
    private _formBuilder: FormBuilder,//表單建立
  ) {
    this.imageForm = this._formBuilder.group({});
    this.getAnnouncement();
  }

  // 查詢頁面顯示內容
  getAnnouncement() {
    this._service.getBlooperImages().subscribe({
      next: (data) => {
        console.log(data);
        if (data && data.result && data.returnStatus == 1) {
          this.images = data.result.map((item: string) => ({
            url: item,
            file: null
          }));
          // this.myDataSource = new MatTableDataSource(data.result?.gridRESPResult);
        } else {
          console.log(data);
        }
      }, error: (error) => console.log(error)
    });
  }

  onFileChange(event: any, index: number) {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      this.images[index].file = file;
    }
  }

  onSubmit() {
    const formData = new FormData();
    this.images.forEach((image, index) => {
      formData.append(`fileArray[${index}]`, image.file || image.url);
    });
    console.log(formData)
    console.log(formData)

    // 送出表單到 API
    this._service.putBlooperImages(formData).subscribe({
      next: (data) => {
        console.log(data);
        if (data && data.result && data.returnStatus == 1) {
          alert("成功");
          this.getAnnouncement();
        } else {
          alert(data.returnMsg)
        }
      }, error: (error) => console.log(error)
    });
  }

}
