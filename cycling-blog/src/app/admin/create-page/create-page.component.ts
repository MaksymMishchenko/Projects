import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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

  get title() { return this.form.get('title'); }
  get category() { return this.form.get('selectCategory')?.get('category'); }
  get description() { return this.form.get('description'); }
  get text() { return this.form.get('text'); }
  get image() { return this.form.get('image'); }
  get author() { return this.form.get('author'); }

  constructor(
    private postsService: PostsService,
    private alert: AlertService,
    private fb: FormBuilder,
    private router: Router
  ) { }

  ngOnInit() {
    this.form = this.fb.group({
      title: new FormControl('', [Validators.required]),
      selectCategory: this.fb.group({
        category: new FormControl('', [Validators.required])
      }),
      description: new FormControl('', [Validators.required]),
      text: new FormControl('', [Validators.required]),
      image: new FormControl('', [Validators.required, Validators.pattern('(https?://)?([\\da-z.-]+)\\.([a-z.]{2,6})[/\\w .-]*/?')]),
      author: new FormControl('', [Validators.required])
    });
  }

  submit() {
    if (this.form.invalid) {
      return;
    }

    const post: Post = {
      title: this.form.value.title,
      category: this.category?.value,
      description: this.form.value.description,
      text: this.form.value.text,
      image: this.form.value.image,
      author: this.form.value.author,
      date: new Date()
    };

    this.postsService.create(post).subscribe(() => {
      this.form.reset();
      this.router.navigate(['/admin', 'dashboard']);
      this.alert.success('Post was created');
    });
  }
}
