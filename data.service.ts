
import { Injectable } from '@angular/core';
import { HttpParams, HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { ErrorLog, ResourceErrors, ResourceRoles, policyRoles, userRoles, DBAPILogs } from './roles';
import { UrlConstructorService } from './services/urlconstructor.service';
@Injectable({
  providedIn: 'root'
})
export class DataService {
  constructor(private http: HttpClient) { }

  getPolicies(): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/PolicyRoles`;
    let data = this.http.get<policyRoles>(url).
      pipe(
        map((res: policyRoles) => {
          return res.data;
        }), catchError(error => {
          return throwError('Something went wrong!');
        })
      );
    return data;
  }


  getAllResourceColumns(): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/AllColumns`;
    console.log(url);
    try {
      return this.http.get<string>(url).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }
  getAllRoles(): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/SecurityRoles`;
    return this.http.get<userRoles>(url).
      pipe(
        map((res: userRoles) => {
          return ['', ...res.data];
        }), catchError(error => {
          return throwError('Something went wrong!');
        })
      );
  }

  getResourceNamesByRole(rolecode: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/ResourceNames?roleCode=${rolecode}`;
    console.log(url);
    try {
      return this.http.get<string>(url).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

  getResources(rolecode: string, resourceName: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/ResourceColumns?roleCode=${rolecode}&resourceName=${resourceName}`;
    return this.http.get<ResourceRoles>(url).
      pipe(
        map((res: ResourceRoles) => {
          return res.data;
        }), catchError(error => {
          return throwError('Something went wrong!');
        })
      );
  }

  getErrorLogById(errorLogId: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v2/ErrorLog/${errorLogId}`;
    return this.http.get<ErrorLog>(url).
      pipe(
        map((res: ErrorLog) => {
          return res;
        }), catchError(error => {
          return throwError('Something went wrong!');
        })
      );
  }

  getErrorLog(errorSearchText: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v2/ErrorLog/?freeText=${errorSearchText}&pageNum=1&pageSize=50`;
    return this.http.get<ResourceErrors>(url).
      pipe(
        map((res: ResourceErrors) => {
          return res.data;
        }), catchError(error => {
          return throwError('Something went wrong!');
        })
      );
  }

  getDBAPILogs(searchText: string, userID: string, logID: string, startDate: Date, endDate: Date, pageRows: number, pageNum: number, orderBy: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/DBAPILogs/search`;
    let params = (new HttpParams())
      .set('searchText', `${searchText}`)
      .append('userId', `${userID}`)
      .append('logId', `${logID}`)
      .append('startDate', `${startDate}`)
      .append('endDate', `${endDate}`)
      .append('pageRows', `${pageRows}`)
      .append('pageNum', `${pageNum}`)
      .append('orderBy', `${orderBy}`);

    try {
      return this.http.get<any>(url, { params: params }).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

  getRoles(): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/SecurityRoles`;
    try {
      return this.http.get<any>(url).pipe(map(res => {
        if (res)
          return res.data;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

  getResourcesByRole(role: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/ResourceColumns?roleCode=${role}`;
    try {
      return this.http.get<any>(url).pipe(map(res => {
        if (res)
          return res.data;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

  getColumnResources(rolecode: string, resourceName: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/ResourceColumns?roleCode=${rolecode}&resourceName=${resourceName}`;
    console.log(url);
    try {
      return this.http.get<any>(url).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }


  getFilteredResources(rolecode: string, resourceName: string, columnName: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v2/SecurityResoures/FilteredResources?roleCode=${rolecode}&resourceName=${resourceName}&columnName=${columnName}`;
    console.log(url);
    try {
      return this.http.get<any>(url).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

 

  getDBAPILogDetail(logID: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/DBAPILogs/${logID}`;

    try {
      return this.http.get<any>(url).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

  getPolicyRoles(): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/PolicyRoles`;
    /*
    let params = (new HttpParams())
      .set('searchText', `${searchText}`)
      .append('startDate', `${startDate}`)
      .append('endDate', `${endDate}`)
      .append('pageRows', `${pageRows}`)
      .append('pageNum', `${pageNum}`);
    */
    try {
      return this.http.get<any>(url).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

deleteRolePolicy(roleCode: string, policyKey: string): Observable<any> {
    const url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/PolicyRoles`;
    const body = {
      RoleCode: roleCode,
      PolicyKey: policyKey
    };

    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'accept': 'application/octet-stream'
      }),
      body: body
    };

    return this.http.delete<any>(url, options).pipe(
        map((res: ResourceErrors) => {
          return res.data;
        }), catchError(error => {
          return throwError('unable to delete role policy!');
        })
      );
  }

  addRolePolicy(roleCode: string, policyKey: string): Observable<any> {
    const url = `${UrlConstructorService.apiUrl}/v1/SecurityResoures/PolicyRoles`;
    const body = {
      RoleCode: roleCode,
      PolicyKey: policyKey
    };

    const options = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'accept': 'application/octet-stream'
      }),
      body: body
    };

 
    return this.http.post<any>(url, body, options).pipe(
        map((res: any) => {
          return res.data;
        }), catchError(error => {
          return throwError('unable to add role policy!');
        })
      );
  }

  /**Debug Users */
  getDebugUsers(pageRows: number, pageNum: number, orderBy: string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v2/DebugUsers/search`;
    let params = (new HttpParams())
      .set('IncludeColumns', `WDADNAMETEXT`)
      .append('IncludeColumns', `WDCREATEID`)
      .append('IncludeColumns', `WDCREATEDATE`)
      .append('RowsPerPage', `${pageRows}`)
      .append('PageNumber', `${pageNum}`)
      .append('OrderBy', `${orderBy}`);

    try {
      return this.http.get<any>(url, { params: params }).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }

  insertDebugUser(debugUser : string): Observable<any> {
    let url = `${UrlConstructorService.apiUrl}/v2/DebugUsers`;
    let insertUser = {
      "debugUserName": debugUser
    }

    try {
      return this.http.post<any>(url, insertUser).pipe(map(res => {
        if (res)
          return res;
        return null;
      }
      ));
    }
    catch (error) {
      console.error(error);
      return error;
    }
  }      

  deleteDebugUser(debugUser : string) : Observable<any>{
      let url = `${UrlConstructorService.apiUrl}/v1/DebugUsers/?debugUserName=${debugUser}`;
      try {
        return this.http.delete<any>(url).pipe(map(res => {
          if (res)
            return res;
          return null;
        }
        ));
      }
      catch (error) {
        console.error(error);
        return error;
      }
    }
}