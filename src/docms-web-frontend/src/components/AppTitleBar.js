import React from 'react';
import { withRouter } from 'react-router-dom';
import { AppBar, Button, Toolbar, Typography } from '@material-ui/core';
import { makeStyles } from '@material-ui/core/styles';
import AppContext from '../AppContext';


const useStyles = makeStyles(theme => ({
  title: {
    flexGrow: 1,
  },
}));

const AuthButton = withRouter(
  ({ history }) => {
    return (
      <AppContext.Consumer>
        {context => {
          const auth = context.getState().auth;
          return auth.isAuthenticated
            ? (<Button color="inherit" onClick={() => auth.signout(() => history.push('/'))}>LOGOUT</Button>)
            : null
        }}
      </AppContext.Consumer>
    );
  }
);

function AppTitleBar() {
  const classes = useStyles();
  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" className={classes.title}>
          文書管理システム
        </Typography>
        <AuthButton />
      </Toolbar>
    </AppBar>
  );
}

export default AppTitleBar;
