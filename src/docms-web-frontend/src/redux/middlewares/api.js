import { LOGIN_REQUESTED, authenticated, loginDenied } from "../actions";
import { REQUEST_DOCUMENT, setDocument, documentNotFound } from "../actions/documents";
import { saveState } from "../actions/persistence";

const api = ({ dispatch }) => next => action => {
  next(action);

  if (action.type === 'API') {
    let { url, request, success, fail } = action.payload;
    let httpRequestOptions = {
      headers: {
        'Accept': 'application/json'
      }
    };
    if (request) {
      // httpRequestOptions.method = request.method || 'GET';
      httpRequestOptions.method = 'GET';
      if (request.headers) {
        for (const key in request.headers) {
          if (request.headers.hasOwnProperty(key)) {
            httpRequestOptions.headers[key] = request.headers[key];
          }
        }
      }
      if (request.body) {
        let param = new URLSearchParams();
        param['Content-Type'] = 'application/x-www-form-urlencoded';
        for (const key in request.body) {
          if (request.body.hasOwnProperty(key)) {
            param.set(key, request.body[key]);
          }
        }
        if (httpRequestOptions.method === 'GET') {
          if (url.includes('?')) {
            url = url + '&' + param;
          } else {
            url = url + '?' + param;
          }
        } else {
          httpRequestOptions.body = param;
        }
      }
    }

    fetch(url, httpRequestOptions)
      .then(response => {
        if (response.ok) {
          return response.json();
        }
        throw new Error('status code is not ok. status code: ' + response.status)
      })
      .then(json => {
        dispatch(success(json));
        dispatch(saveState());
      })
      .catch(error => dispatch(fail(error)));
  }
}

const login = store => next => action => {
  next(action);

  if (action.type === LOGIN_REQUESTED) {
    const { userName, password } = action.payload;
    store.dispatch({
      type: 'API',
      payload: {
        url: 'http://localhost:3000/account/login.json',
        request: {
          method: 'POST',
          body: {
            userName,
            password
          }
        },
        success: authenticated,
        fail: loginDenied
      }
    });
  }
}


const document = ({ dispatch }) => next => action => {
  next(action);

  if (action.type === REQUEST_DOCUMENT) {
    let { path } = action.payload;
    if (path) {
      path = path + '/';
    }

    dispatch({
      type: 'API',
      payload: {
        url: '/api/docs/' + path + 'index.json',
        success: data => setDocument(data),
        fail: error => documentNotFound(error, path)
      }
    })
  }
}

export default [api, login, document];