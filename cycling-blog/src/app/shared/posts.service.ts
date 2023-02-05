import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { environment } from "../admin/shared/env";
import { Post } from "./interfaces";

@Injectable({ providedIn: 'root' })

export class PostsService {
    constructor(private http: HttpClient) { }

    create(post: Post): Observable<Post> {
        return this.http.post<Post>(`${environment.firebaseDbUrl}/posts.json`, post)
            .pipe(map((response: any) => {
                return {
                    ...post,
                    id: response.name,
                    date: new Date(post.date)
                }
            }));
    }
}