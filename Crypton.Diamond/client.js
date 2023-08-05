const SHA1 = f => {function $(f,$){return f<<$|f>>>32-$}function r(f){var $,r,o,e="";for($=0;$<=6;$+=2)r=f>>>4*$+4&15,o=f>>>4*$&15,e+=r.toString(16)+o.toString(16);return e}function o(f){var $,r,o="";for($=7;$>=0;$--)o+=(r=f>>>4*$&15).toString(16);return o}var e,_,t,a,C,h,x,n,c,d=Array(80),A=1732584193,u=4023233417,s=2562383102,i=271733878,g=3285377520,m=(f=function f($){$=$.replace(/\r\n/g,"\n");for(var r="",o=0;o<$.length;o++){var e=$.charCodeAt(o);e<128?r+=String.fromCharCode(e):e>127&&e<2048?(r+=String.fromCharCode(e>>6|192),r+=String.fromCharCode(63&e|128)):(r+=String.fromCharCode(e>>12|224),r+=String.fromCharCode(e>>6&63|128),r+=String.fromCharCode(63&e|128))}return r}(f)).length,p=[];for(_=0;_<m-3;_+=4)t=f.charCodeAt(_)<<24|f.charCodeAt(_+1)<<16|f.charCodeAt(_+2)<<8|f.charCodeAt(_+3),p.push(t);switch(m%4){case 0:_=2147483648;break;case 1:_=f.charCodeAt(m-1)<<24|8388608;break;case 2:_=f.charCodeAt(m-2)<<24|f.charCodeAt(m-1)<<16|32768;break;case 3:_=f.charCodeAt(m-3)<<24|f.charCodeAt(m-2)<<16|f.charCodeAt(m-1)<<8|128}for(p.push(_);p.length%16!=14;)p.push(0);for(p.push(m>>>29),p.push(m<<3&4294967295),e=0;e<p.length;e+=16){for(_=0;_<16;_++)d[_]=p[e+_];for(_=16;_<=79;_++)d[_]=$(d[_-3]^d[_-8]^d[_-14]^d[_-16],1);for(_=0,a=A,C=u,h=s,x=i,n=g;_<=19;_++)c=$(a,5)+(C&h|~C&x)+n+d[_]+1518500249&4294967295,n=x,x=h,h=$(C,30),C=a,a=c;for(_=20;_<=39;_++)c=$(a,5)+(C^h^x)+n+d[_]+1859775393&4294967295,n=x,x=h,h=$(C,30),C=a,a=c;for(_=40;_<=59;_++)c=$(a,5)+(C&h|C&x|h&x)+n+d[_]+2400959708&4294967295,n=x,x=h,h=$(C,30),C=a,a=c;for(_=60;_<=79;_++)c=$(a,5)+(C^h^x)+n+d[_]+3395469782&4294967295,n=x,x=h,h=$(C,30),C=a,a=c;A=A+a&4294967295,u=u+C&4294967295,s=s+h&4294967295,i=i+x&4294967295,g=g+n&4294967295}var c=o(A)+o(u)+o(s)+o(i)+o(g);return c.toLowerCase()}


class Rules {
    /**
     * the rules to sign the request
     * @param {string} salt the salt to hash the request
     * @param {string} app_token the application token
     * @param {number[]} checksum_indexes the indexes to get the checksum
     * @param {number} checksum_start the start of the checksum
     * @param {string} hash_format the format of the hash
     */
    constructor(salt, app_token, checksum_indexes, checksum_start, hash_format) {
        this.salt = salt;
        this.app_token = app_token;
        this.checksum_indexes = checksum_indexes;
        this.checksum_start = checksum_start;
        this.hash_format = hash_format;
    }

    /**
     * fetch from api
     * @returns {Rules}
     */
    static async fetchRules() {
        return await fetch("/api/v1/auth/rules").then(response => response.json());
    }
}

/**
 * sign the request
 * @param {Rules} rules the rules to sign the request
 * @param {string} url the url to fetch
 * @param {string} userId the user id who performed the action
 * @param {Date} timestamp the timestamp of the request
 * @returns {string} the signed url
 */
const sign = (rules, url, userId, timestamp) => {
    if (url == null || userId == null)
        return null;

    const parsedUri = new URL(url);
    const parsedUrl = `${parsedUri.pathname}${parsedUri.search}`;

    const payload = `${rules.salt}:${timestamp.toISOString()}:${parsedUrl}:${userId}`;

    const digest = SHA1(payload);

    const digestBytes = new TextEncoder().encode(digest);

    let sum = rules.checksum_start;
    for (let i = 0; i < rules.checksum_indexes.length; i++) {
        const idx = rules.checksum_indexes[i];
        sum += digestBytes[idx] % 256;
    }

    sum = Math.abs(sum % 256);
    console.log(sum);

    // pad the sum so its always 3 digits
    const sumString = sum.toString().padStart(3, "0");

    return rules.hash_format
        .replace("{0}", digest)
        .replace("{1}", sumString);
}


/**
 * make a request to the server, and sign it with the application rules
 * @param {string} url the url to fetch
 * @param {string} userId the user id who performed the action
 * @returns {Promise<Response>} the response
 */
const request = async (url, userId) => {
    const rules = await Rules.fetchRules();
    const timestamp = new Date();

    const signature = sign(rules, url, userId, timestamp);
    console.log(signature);

    return await fetch(url, {
        headers: {
            "X-App-Token": rules.app_token,
            "X-User-Id": userId,
            "X-Timestamp": timestamp.toISOString(),
            "X-Signature": signature,
        }
    });
}

const USER_ID = "accbca9d-ee2b-4ea9-be78-a5d951eb67e6"
const API_URL = "https://localhost:5001/api/v1/auth/me"

request(API_URL, USER_ID)
    .then(response => response.text())
    .then(text => console.log(text));