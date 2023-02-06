import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, Observable, pipe, tap, throwError } from "rxjs";
import { AuthService } from "../admin/shared/services/auth.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private auth: AuthService, private router: Router) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (this.auth.isAuthenticated()) {
            req = req.clone({
                setParams: {
                    auth: this.auth.token
                }
            });
        };

        return next.handle(req)
        .pipe(
            catchError((error: HttpErrorResponse) => {
                console.log('InterceptorError', error)
                if(error.status === 401) {
                    this.auth.logout();
                    this.router.navigate(['/admin', 'login'], {
                        queryParams: {
                            sessionTimedOut: true
                        }
                    });
                };
                return throwError(()=> new Error(error.toString()))
            })
        );
    }
}