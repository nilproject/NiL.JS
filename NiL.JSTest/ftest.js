function f() {
    return { x: 1 };
}
console.log(new f().x)
console.log(this.x);