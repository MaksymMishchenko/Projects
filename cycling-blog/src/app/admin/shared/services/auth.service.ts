import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { FireBaseResponse, User } from "src/app/shared/interfaces";
import { Observable, tap } from "rxjs";
import { environment } from "../env";

@Injectable()

export class AuthService {
    constructor(private http: HttpClient) { }

    get token(): string {
        const expDate = new Date(localStorage.getItem('fb-token-exp')!);
        if (new Date() > expDate) {
            this.logout();
            return null!;
        }
        return localStorage!.getItem('fb-token')!;
    }

    login(user: User): Observable<any> {
        user.returnSecureToken = true
        return this.http.post(`https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=${environment.apiKey}`, user)
            .pipe(
                tap(this.setToken)
            );
    }

    logout() {
        this.setToken(null);
    }

    isAuthenticated(): boolean {
        return !!this.token;
    }

    private setToken(response: any) {
        if (response) {
            const expDate = new Date(new Date().getTime() + +response.expiresIn * 1000);
            localStorage.setItem('fb-token', response.idToken);
            localStorage.setItem('fb-token-exp', expDate.toString());
        } else {
            localStorage.clear();
        }

    }
}