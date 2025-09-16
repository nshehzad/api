import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})

export class UrlConstructorService {
  static get origin() {
    return location.origin.toLowerCase();
  }

  static get environmentName() {
    if (origin.includes('localhost'))
      return 'local';
    let parts = window.location.href.toLowerCase().replace(origin, '').split('/').filter(a => a);
    let env = parts.length > 0 ? parts[0] : '';
    console.log('env', env);
    return env;
  }

  static get baseHref() {
    let val = UrlConstructorService.environmentName == 'local' ? '' : origin + '/' + UrlConstructorService.environmentName;
    console.log('baseHref', val);
    return val;
  }

  static get applicationUrl() {
    let val = UrlConstructorService.environmentName == 'local' ? origin :
      (origin + '/' + UrlConstructorService.environmentName + '/GTMData_Admin');
    console.log('applicationUrl', val);
    return val;
  }

  static get apiUrl() {
   // return UrlConstructorService.environmentName == "local" ? "https://gtmdev1hronline.hr.dosdev.us/Dev1/GTMAdminApi/api" :
        return UrlConstructorService.environmentName == "local"?  "https://localhost:50722/api" :  
      UrlConstructorService.origin + '/' + UrlConstructorService.environmentName + '/GTMAdminApi/api';
  }

  static get enterpriseAPIUrl() {
    return UrlConstructorService.environmentName == "local" ? "https://gtmdev1hronline.hr.dosdev.us/Dev1/gtmdata/api" :
      UrlConstructorService.origin + '/' + UrlConstructorService.environmentName + '/gtmdata/api';
  }

  static get authIssuer() {
    return UrlConstructorService.environmentName == "local" ? "https://gtmdev1hronline.hr.dosdev.us/dev1/gtmsts" : UrlConstructorService.baseHref + '/gtmsts';
  }
}
