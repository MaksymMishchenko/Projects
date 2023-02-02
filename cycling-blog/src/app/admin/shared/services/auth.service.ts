import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { FireBaseResponse, User } from "src/app/shared/interfaces";
import { Observable, tap } from "rxjs";
import { environment } from "../env";

@Injectable()

export class AuthService {
    constructor(private http: HttpClient) { }

    get token(): string {
        return '';
    }

    login(user: User): Observable<any> {
        return this.http.post(`https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=${environment.apiKey}`, user)
            .pipe(
                tap(this.setToken)
            );
    }

    logout() {

    }

    isAuthenticated(): boolean {
        return !!this.token
    }

    private setToken(response: FireBaseResponse) {
        console.log(response)
    }
}