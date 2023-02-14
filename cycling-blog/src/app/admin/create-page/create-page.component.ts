import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Post } from 'src/app/shared/interfaces';
import { PostsService } from 'src/app/shared/posts.service';
import { AlertService } from '../shared/services/alert.service';

@Component({
  selector: 'app-create-page',
  templateUrl: './create-page.component.html',
  styleUrls: ['./create-page.component.scss']
})
export class CreatePageComponent implements OnInit {

  form!: FormGroup;

  constructor(
    private postsService: PostsService,
    private alert: AlertService,
    private fb: FormBuilder
    ) { }

  ngOnInit() {
    this.form = this.fb.group({
      title: new FormControl('', [Validators.required]),
      selectCategory: this.fb.group({
        category: new FormControl('', [Validators.required])
      }),
      description: new FormControl('', [Validators.required]),
      text: new FormControl('', [Validators.required]),
      image: new FormControl('', [Validators.required]),
      author: new FormControl('', [Validators.required])
    });
  }

  submit() {
    if (this.form.invalid) {
      return;
    }

    const post: Post = {
      title: this.form.value.title,
      category: this.form.get('selectCategory')?.get('category')?.value,
      description: this.form.value.description,
      text: this.form.value.text,
      image: this.form.value.image,
      author: this.form.value.author,
      date: new Date()
    }

    this.postsService.create(post).subscribe(() => {
      this.form.reset();
      this.alert.success('Post was created')
    });
  }
}
