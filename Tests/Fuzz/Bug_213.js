/**
 * @onlyStrict
 */

foo();
function foo(){}
foo();

poo();
export function poo(){}
poo();

bar();
export default function bar(){}
bar();

export default class {
    async build() {
        const obj = await Promise.resolve(123);
        return obj;
    }
};

export default class Builder {
    async build() {
        const obj = await Promise.resolve(123);
        return obj;
    }
};

(new Builder()).build().then(function () { });