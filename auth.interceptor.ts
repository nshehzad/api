import { Observable } from 'rxjs';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';


@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor() {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
   
    // Check origin portion only
    var location = window.location.origin.toLowerCase();

    if (location.includes('msappproxy.net') &&  request.headers.get("authorization"))    {
              // If this is Go Browser and the request contains Authorization header, then add a custom header to pass the JWT
              request = request.clone({      
                  setHeaders:{"Access-Control-Allow-Origin": location, "Authorization-Bearer-JWT": request.headers.get("authorization")},  
                  withCredentials: true
              });   
              console.log ('Add Authorization-Bearer-JWT request header for GO Browser');
          }
    else {
      request = request.clone({      
        setHeaders:{"Access-Control-Allow-Origin": location},
        withCredentials: true
      });   
    }
          
           
    return next.handle(request);
  } 
}
