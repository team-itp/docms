import React from 'react';
import AuthService from './services/AuthService';

const AppContext = React.createContext({
    auth: new AuthService()
});

export default AppContext;