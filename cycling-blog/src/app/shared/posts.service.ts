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

    getAllPosts(): Observable<Post[]> {
        return this.http.get<Post>(`${environment.firebaseDbUrl}/posts.json`)
            .pipe(map((response: { [key: string]: any }) => {
                return Object.keys(response)
                    .map(key => ({
                        ...response[key],
                        id: key,
                        date: new Date(response[key].date)
                    }))
            }));
    }

    remove(id: any): Observable<void> {
        return this.http.delete<void>(`${environment.firebaseDbUrl}/posts/${id}.json`);
    }
}