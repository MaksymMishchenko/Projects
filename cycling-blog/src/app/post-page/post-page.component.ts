import { Component, OnInit } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRoute, Params } from '@angular/router';
import { Observable, switchMap } from 'rxjs';
import { Post } from '../shared/interfaces';
import { PostsService } from '../shared/posts.service';

@Component({
  selector: 'app-post-page',
  templateUrl: './post-page.component.html',
  styleUrls: ['./post-page.component.scss']
})
export class PostPageComponent implements OnInit {

  post$!: Observable<Post>;
  constructor(private route: ActivatedRoute,
    private postsService: PostsService,
    private title: Title, private meta: Meta) { }

  ngOnInit() {
    this.post$ = this.route.params.pipe(switchMap((params: Params) => {
      return this.postsService.getPostById(params['id'])
    }));
    this.post$.subscribe((post) => {
      this.title.setTitle(post.metaTitle);
      this.meta.addTags([{
        name: 'description', content: post.metaDescription
      }]);
    });
  }
}
