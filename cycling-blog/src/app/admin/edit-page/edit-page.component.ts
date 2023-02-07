import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Params } from '@angular/router';
import { Subscribable, Subscription, switchMap } from 'rxjs';
import { Post } from 'src/app/shared/interfaces';
import { PostsService } from 'src/app/shared/posts.service';
import { AlertService } from '../shared/services/alert.service';

@Component({
  selector: 'app-edit-page',
  templateUrl: './edit-page.component.html',
  styleUrls: ['./edit-page.component.scss']
})
export class EditPageComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  post!: Post;
  submitted = false;
  uSub!: Subscription

  constructor(private route: ActivatedRoute, private postsService: PostsService, private alert: AlertService) { }

  ngOnInit() {
    this.route.params.pipe(
      switchMap((params: Params) => {
        return this.postsService.getPostById(params['id'])
      })
    ).subscribe((post: Post) => {
      this.post = post
      this.form = new FormGroup({
        title: new FormControl(post.title, [Validators.required]),
        selectCategory: new FormGroup({
          category: new FormControl(post.category, [Validators.required])
        }),
        text: new FormControl(post.text, [Validators.required]),
        image: new FormControl(post.image, [Validators.required]),
        author: new FormControl(post.author, [Validators.required])
      })
    })
  }

  submit() {
    if (this.form.invalid) {
      return;
    }

    this.submitted = true;

    this.uSub = this.postsService.update({
      ...this.post,
      text: this.form.value.text,
      title: this.form.value.title,
      image: this.form.value.image,
      author: this.form.value.author
    }).subscribe(() => {
      this.submitted = false;
      this.alert.success('Post was updated')
    });
  }

  ngOnDestroy() {
    if (this.uSub) {
      this.uSub.unsubscribe()
    }
  }
}
