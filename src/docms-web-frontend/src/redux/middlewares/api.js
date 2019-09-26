import { LOGIN_REQUESTED, authenticated, loginDenied } from "../actions";
import { DOCUMENT_REQUEST } from "../actions/documents";

const api = ({ dispatch }) => next => action => {
  next(action);

  if (action.type === 'API') {
    const { url, success, fail } = action.payload;
    fetch(url)
      .then(
        response => response.json(),
        error => dispatch(fail(error))
      )
      .then(json => dispatch(success(json)));
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
        method: 'POST',
        body: {
          userName,
          password
        },
        success: authenticated,
        fail: loginDenied
      }
    });
  }
}


const document = ({ dispatch }) => next => action => {
  next(action);

  if (action.type === DOCUMENT_REQUEST) {
    const { path, documentReceived, documentNotExists } = action.payload;
    dispatch({
      type: 'API',
      payload: {
        url: '/api/docs' + path + 'index.js',
        method: 'GET',
        success: documentReceived,
        fail: documentNotExists
      }
    })
  }
}

export default [api, login, document];