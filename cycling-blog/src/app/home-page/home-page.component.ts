import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { Post } from '../shared/interfaces';
import { PostsService } from '../shared/posts.service';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss']
})
export class HomePageComponent implements OnInit, OnDestroy {

  posts!: Post[]
  pSub!: Subscription
  postRows!: number
  constructor(private postService: PostsService) { }

  ngOnInit() {
    this.pSub = this.postService.getAllPosts()
      .subscribe(posts => {
        this.posts = posts;
      });
  }

  ngOnDestroy(): void {
    if (this.pSub) {
      this.pSub.unsubscribe();
    }
  }
}
