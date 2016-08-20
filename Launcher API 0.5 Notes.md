Notes on the SAFE Network Launcher API v0.5
===========================================

The [Launcher API](https://maidsafe.readme.io/) exists to ease application development for the [SAFE Network](http://maidsafe.net/).
As per the [introduction](https://maidsafe.readme.io/docs/introduction):
> SAFE Launcher exposes REST APIs for applications to exchange data with the SAFE Network.

Below are a few thoughts and illustrative usage examples based on my experience thus far.
It's partly to document, partly to add my thoughts on the design decisions and current implementation.
Hopefully it can contribute to both those wanting to build things using the Launcher API and share
my ideas on (RESTful) API desig in the process.

I'll give examples using lightly edited raw HTTP request/responses that should be easy to mimic in your 
language of choice. For more involved sampleles expect C## or similar, I'll try to keep it readable.

Let's get started, the first thing any app will need to do is to request Authorization. 

Authorization [/auth](https://maidsafe.readme.io/docs/auth)
===========================================================

In order to do anything beyond accessing public resources each request must request authorization and 
needed permissions. Currently the only extra permission is `SAFE_DRIVE_ACCESS` that grants access to
the data area that can be shared between applications. Without that permission only app local data can
be accessed.

Requesting authorization is a simple matter of POSTing app details and if the user decides to grant
access a token will be passed back that should be sent in for subsequent requests.
```
POST /auth HTTP/1.1
Host: localhost:8100
Content-Type: application/json

{
  "app": {
    "name": "SAFE Launcher API Examples",
    "id": "safenet.examples",
    "version": "0.0.0.1",
    "vendor": "drunkcod"
  },
  "permissions": []
}
```

Given that we decide grant access the reply is similar to this:
Where <token> is an opaque blob of text, just remember to keep track of it.
(It's a JWT token for those curious)
```
200 OK
Content-Type: application/json; charset=utf-8

{
    "token": "<token>",
    "permissions": []
}
```

As can be seen we get a nice json reply containing our access token and the
granted list of permissions. Not particularly useful today but great from 
a completeness perspective and once more granular access is granted it will 
let applications gracefully degrade if not everything requested is granted.

Tokens can be cached and reused between sessions in order to not bug the 
user with repeated authorization requests. This is how the Demo App manages
to only ask for authorization once per Launcher restart.

To check if your <token> is still valid a simple GET is all that's required.
Simply set the Authorization header to "Bearer <token>" and GET /auth 
```
GET /auth HTTP/1.1
Host: localhost:8100
Authorization: Bearer <token>
```

If the token is valid you get a 200 reply:
```
200 OK
Content-Type: text/plain; charset=utf-8

OK
```

If the token has been revoked, or the launcher restarted 
```
GET /auth HTTP/1.1
Host: localhost:8100
Authorization: Bearer invalid-token
```
You'll get a 401 Unauthorised reply
```
401 Unauthorized
Content-Type: text/plain; charset=utf-8

Unauthorized
```

If you forget to set your Authorization header and access a protected endpoint
```
GET /auth HTTP/1.1
Host: localhost:8100
```

The result is as follows
```
401 Unauthorized
Content-Type: application/json; charset=utf-8

{
    "errorCode": 400,
    "description": "Unauthorised"
}
```

## Thoughts
As can be seen above each individual request and response is simple. 
But taken togheter there's a few things that stick out.

### Inconsistent content types for replies. You either get json or text.
From a user perspectie it's rather odd and inconvienent to not be able to make
assumptions regarding what will be coming back over the wire. The decision to
sometimes return json and at other times go for plain text feels like an oversight.

Even without debating the appropriateness of returning non object json types the OK
and Unauthorised responses could have passed for valid json with the inclusion of
some well placed "'s.

### Inconsistent response types / missing information for GET & POST /auth
Token caching is very useful for the user experience, from an app developers
perspective the odd mix or responses between requesting authorzation and checking
the token is a bit problematic. 

Why? When the app requests access permissions are returned and can be checked. 
When reusing a cached token this information is missing. Thus either the application
need to cache not only the token, but also the permissions associated with it or
re-authenticate to get an up to date list. 

An improved design would align the POST & GET return types thus enabling clients
to treat a 200 from both calls using the exact same code.

### Different responses Invalid and Missing Authorization header

The current design gives you different responses for the same semantic error.
Being not authroized gives you either a json object _or_ a string. 
While this difference makes enables knowing if the problem is that no Authorization
header was set or if the token is invalid it makes handling 401's harder. 
For most practical purposes nothing would be lost by sending back the same json
type response for both cases, optionally highligthing the source using either the
description or errorCode. Especially since errorCode doesn't follow the http status.

_To Be Continued_
=================