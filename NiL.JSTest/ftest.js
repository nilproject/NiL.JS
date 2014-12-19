var desc = Object.getOwnPropertyDescriptor(String.prototype, "substr");
if (desc.value === String.prototype.substr &&
    desc.writable === true &&
    desc.enumerable === false &&
    desc.configurable === true) {
    console.log(true);
}
else
    console.log(false);