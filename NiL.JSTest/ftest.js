var x = 1;
function f()
{
    this.x = 2;
}
console.log(new f().x);
console.log(this.x);