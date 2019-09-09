import React from 'react';
import store from './redux/store';
import { login } from './redux/actions';

const AppContext = React.createContext({
    getState: store.getState,
    login: (userName, password) => {
        store.dispatch(login(userName, password));
    }
});

export default AppContext;