import type { ReactNode } from "react";

/** A bordered box with a title bar. Every page and sidebar panel is made of these. */
export function Box({ title, children }: { title: ReactNode; children: ReactNode }) {
    return (
        <section className="box">
            <h2 className="box-title">{title}</h2>
            <div className="box-body">{children}</div>
        </section>
    );
}
