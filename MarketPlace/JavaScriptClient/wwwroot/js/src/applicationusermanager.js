import * as Oidc from 'oidc-client/lib/oidc-client.js'

class ApplicationUserManager extends Oidc.UserManager {
    constructor() {
        super({
            authority: "http://localhost:5002",
            client_id: "js",
            redirect_uri: "http://localhost:5001/callback.html",
            response_type: "id_token token",
            scope: "openid profile MarketplaceService",
            post_logout_redirect_uri: "http://localhost:5001/index.html"
        });

        this.getUser().then(user => {
            if (user)
                this.log("User logged in", user.profile);
            else
                this.log("User not logged in");
        }).catch(error => this.log("Problem trying to read the user", error));
    }

    log(...parameters) {
        for (const parameter of parameters) {
            let msg;
            if (parameter instanceof Error)
                msg = `Error: ${parameter.message}`;
            else if (typeof parameter !== 'string')
                msg = JSON.stringify(parameter, null, 2);
            else
                msg = parameter;
            console.log(`${msg} \r\n`);
        }
    }

    async login() {
        await this.signinRedirect();
        return await this.getUser();
    }

    async logout() {
        return await this.signoutRedirect();
    }
}

const applicationUserManager = new ApplicationUserManager();
export { applicationUserManager as default };


