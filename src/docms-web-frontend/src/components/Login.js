import React from 'react';
import { Redirect } from 'react-router-dom';
import { Button, Container, FormControlLabel, Grid, Switch, Typography, TextField } from '@material-ui/core';
import AppContext from '../AppContext';

class Login extends React.Component {
  static contextType = AppContext;

  constructor(props) {
    super(props);
    this.state = { redirectToReferrer: false };
    this.login = this.login.bind(this);
  }

  login() {
    const auth = this.context.auth;
    auth.authenticate(() => {
      this.setState({ redirectToReferrer: true });
    });
  }

  render() {
    const from = this.props.location.state || { from: { pathname: "/" } };
    const redirectToReferrer = this.state.redirectToReferrer;

    if (redirectToReferrer) return <Redirect to={from} />;

    return (
      <Container component="main" maxWidth="lx" style={{ marginTop: '20px' }}>
        <Grid alignItems="center">
          <Typography component="h2" variant="h5">
            ログイン
          </Typography>
        </Grid>
        <form>
          <Grid container>
            <Grid item sm={6}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextField
                    required
                    fullWidth
                    label="ユーザー名" />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    required
                    fullWidth
                    type="password"
                    label="パスワード" />
                </Grid>
                <Grid item xs={12}>
                  <FormControlLabel control={
                    <Switch />
                  } label="サインイン状態を保存する" />
                </Grid>
                <Grid item xs={12}>
                  <Button onClick={this.login} color="primary" variant="outlined">ログイン</Button>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
        </form>
      </Container>
    );
  }
}

export default Login;