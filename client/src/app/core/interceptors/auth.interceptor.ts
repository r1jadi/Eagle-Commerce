import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const clonedReuest = req.clone({
    withCredentials: true
  })


  return next(clonedReuest);
};
