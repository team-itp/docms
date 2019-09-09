import React from 'react';
import { Switch, BrowserRouter as Router, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { createMuiTheme } from '@material-ui/core/styles';
import { blueGrey, orange } from '@material-ui/core/colors';
import { ThemeProvider } from '@material-ui/styles';
import store from './redux/store';
import ProtectedRoute from './components/ProtectedRoute';
import AppTitleBar from './components/AppTitleBar';
import Login from './components/Login';
import Home from './components/Home';
import NotFound from './components/NotFound';
import DocumentBrowser from './components/DocumentBrowser';

const theme = createMuiTheme({
  palette: {
    primary: {
      main: blueGrey['800'],
    },
    secondary: {
      main: orange['700'],
    },
  },
});

class App extends React.Component {
  render() {
    return (
      <ThemeProvider theme={theme}>
        <Provider store={store}>
          <Router>
            <div className="App">
              <AppTitleBar />
              <Switch>
                <Route path="/login" component={Login} />
                <ProtectedRoute path="/" exact component={Home} />
                <ProtectedRoute path="/docs/*" component={DocumentBrowser} />
                <Route component={NotFound} />
              </Switch>
            </div>
          </Router>
        </Provider>
      </ThemeProvider>
    );
  }
}

export default App;
